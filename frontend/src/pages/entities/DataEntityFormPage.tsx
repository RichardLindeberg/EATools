import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useController, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { apiClient } from '../../api/client';
import { DataEntityFormSchema, DataEntityFormData } from '../../utils/formValidation';
import { useEntity } from '../../hooks/useEntity';
import { EntityFormTemplate } from '../../components/forms/EntityFormTemplate';
import { FormFieldWrapper } from '../../components/forms/FormFieldWrapper';
import { DynamicFieldArray } from '../../components/forms/DynamicFieldArray';
import { DiscardChangesModal } from '../../components/forms/DiscardChangesModal';
import { hasUnsavedChanges, deepClone } from '../../utils/formHelpers';
import './EntityFormPage.css';

interface DataEntityFormPageProps {
  isEdit?: boolean;
}

export function DataEntityFormPage({ isEdit = false }: DataEntityFormPageProps) {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [showDiscardModal, setShowDiscardModal] = useState(false);

  const { data: existingDataEntity, isLoading: isLoadingDataEntity } = useEntity(
    isEdit ? 'data-entities' : '',
    isEdit ? id : ''
  );

  const {
    register,
    control,
    watch,
    handleSubmit: handleReactFormSubmit,
    formState: { errors, isSubmitting },
    setError,
    reset,
  } = useForm<DataEntityFormData>({
    resolver: zodResolver(DataEntityFormSchema),
    defaultValues: isEdit
      ? existingDataEntity || {}
      : {
          name: '',
          description: '',
          owner: '',
          classification: 'Internal',
          sensitivityLevel: 'Medium',
          hasPii: false,
          encrypted: false,
          retentionPeriod: '5y',
          retentionUnit: 'Years',
          systemOfRecord: '',
          relatedSystems: [],
          backupRequired: false,
          backupFrequency: '',
          complianceRules: [],
          tags: [],
        },
  });

  const { field: relatedSystemsField } = useController({
    control,
    name: 'relatedSystems',
  });

  const { field: complianceRulesField } = useController({
    control,
    name: 'complianceRules',
  });

  const { field: tagsField } = useController({
    control,
    name: 'tags',
  });

  const watchedValues = watch();
  const [originalValues] = useState(() => deepClone(watchedValues));
  const hasChanges = hasUnsavedChanges(originalValues, watchedValues);

  useEffect(() => {
    if (existingDataEntity) {
      reset(existingDataEntity);
    }
  }, [existingDataEntity, reset]);

  const onSubmit = async (data: DataEntityFormData) => {
    try {
      let response;

      if (isEdit && id) {
        response = await apiClient.patch(`/data-entities/${id}`, data);
      } else {
        response = await apiClient.post('/data-entities', data);
      }

      if (response.data?.id) {
        navigate(`/entities/data-entities/${response.data.id}`);
      }
    } catch (error: any) {
      const errorMessage =
        error?.response?.data?.detail || error?.message || 'Failed to save data entity';

      if (error?.response?.status === 422 && error?.response?.data?.errors) {
        error.response.data.errors.forEach((err: any) => {
          if (err.field) {
            setError(err.field as any, {
              type: 'server',
              message: err.message,
            });
          }
        });
      }
    }
  };

  const handleCancel = () => {
    if (hasChanges) {
      setShowDiscardModal(true);
    } else {
      navigate('/entities/data-entities');
    }
  };

  const handleConfirmDiscard = () => {
    setShowDiscardModal(false);
    navigate('/entities/data-entities');
  };

  if (isEdit && isLoadingDataEntity) {
    return <div className="entity-form-loading">Loading...</div>;
  }

  return (
    <>
      <EntityFormTemplate
        title={isEdit ? 'Edit Data Entity' : 'Create Data Entity'}
        subtitle={
          isEdit ? `Editing: ${existingDataEntity?.name || ''}` : 'Add a new data entity'
        }
        isEdit={isEdit}
        isLoading={isSubmitting}
        error={null}
        onSubmit={handleReactFormSubmit(onSubmit)}
        onCancel={handleCancel}
        submitButtonLabel={isEdit ? 'Save Changes' : 'Create Data Entity'}
      >
        <div className="entity-form-section">
          <h3 className="entity-form-section-title">Basic Information</h3>

          <div className="entity-form-section-grid">
            <FormFieldWrapper
              label="Data Entity Name"
              required
              error={errors.name?.message}
              htmlFor="name"
            >
              <input
                id="name"
                type="text"
                placeholder="e.g., Customer Records"
                {...register('name')}
              />
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Owner"
              required
              error={errors.owner?.message}
              htmlFor="owner"
            >
              <input
                id="owner"
                type="text"
                placeholder="Enter owner department"
                {...register('owner')}
              />
            </FormFieldWrapper>

            <FormFieldWrapper
              label="System of Record"
              required
              error={errors.systemOfRecord?.message}
              htmlFor="systemOfRecord"
            >
              <input
                id="systemOfRecord"
                type="text"
                placeholder="e.g., Salesforce"
                {...register('systemOfRecord')}
              />
            </FormFieldWrapper>
          </div>

          <FormFieldWrapper
            label="Description"
            error={errors.description?.message}
            htmlFor="description"
          >
            <textarea
              id="description"
              placeholder="Describe the data entity..."
              {...register('description')}
            />
          </FormFieldWrapper>
        </div>

        <div className="entity-form-section">
          <h3 className="entity-form-section-title">Classification & Security</h3>

          <div className="entity-form-section-grid">
            <FormFieldWrapper
              label="Classification"
              required
              error={errors.classification?.message}
              htmlFor="classification"
            >
              <select id="classification" {...register('classification')}>
                <option value="Public">Public</option>
                <option value="Internal">Internal</option>
                <option value="Confidential">Confidential</option>
                <option value="Restricted">Restricted</option>
              </select>
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Sensitivity Level"
              error={errors.sensitivityLevel?.message}
              htmlFor="sensitivityLevel"
            >
              <select id="sensitivityLevel" {...register('sensitivityLevel')}>
                <option value="Low">Low</option>
                <option value="Medium">Medium</option>
                <option value="High">High</option>
              </select>
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Retention Period"
              required
              error={errors.retentionPeriod?.message}
              htmlFor="retentionPeriod"
            >
              <select id="retentionPeriod" {...register('retentionPeriod')}>
                <option value="1y">1 Year</option>
                <option value="3y">3 Years</option>
                <option value="5y">5 Years</option>
                <option value="7y">7 Years</option>
                <option value="Indefinite">Indefinite</option>
              </select>
            </FormFieldWrapper>
          </div>

          <div className="entity-form-section-grid">
            <FormFieldWrapper label="Contains PII" error={errors.hasPii?.message}>
              <label className="checkbox-label">
                <input type="checkbox" {...register('hasPii')} />
                <span>This data entity contains Personally Identifiable Information</span>
              </label>
            </FormFieldWrapper>

            <FormFieldWrapper label="Encrypted" error={errors.encrypted?.message}>
              <label className="checkbox-label">
                <input type="checkbox" {...register('encrypted')} />
                <span>Data is encrypted at rest</span>
              </label>
            </FormFieldWrapper>

            <FormFieldWrapper label="Backup Required" error={errors.backupRequired?.message}>
              <label className="checkbox-label">
                <input type="checkbox" {...register('backupRequired')} />
                <span>Regular backups are required</span>
              </label>
            </FormFieldWrapper>
          </div>

          <FormFieldWrapper
            label="Backup Frequency"
            error={errors.backupFrequency?.message}
            htmlFor="backupFrequency"
          >
            <input
              id="backupFrequency"
              type="text"
              placeholder="e.g., Daily"
              {...register('backupFrequency')}
            />
          </FormFieldWrapper>
        </div>

        <div className="entity-form-section">
          <h3 className="entity-form-section-title">Related Information</h3>

          <DynamicFieldArray
            label="Related Systems"
            value={relatedSystemsField.value || []}
            onChange={relatedSystemsField.onChange}
            placeholder="e.g., SAP, Salesforce"
            error={errors.relatedSystems?.message}
          />

          <DynamicFieldArray
            label="Compliance Rules"
            value={complianceRulesField.value || []}
            onChange={complianceRulesField.onChange}
            placeholder="e.g., GDPR, HIPAA"
            error={errors.complianceRules?.message}
          />

          <DynamicFieldArray
            label="Tags"
            value={tagsField.value || []}
            onChange={tagsField.onChange}
            placeholder="e.g., customer-data, pii"
            error={errors.tags?.message}
          />
        </div>
      </EntityFormTemplate>

      <DiscardChangesModal
        isOpen={showDiscardModal}
        onConfirm={handleConfirmDiscard}
        onCancel={() => setShowDiscardModal(false)}
      />
    </>
  );
}
