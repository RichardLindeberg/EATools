import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useController, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { apiClient } from '../../api/client';
import { IntegrationFormSchema, IntegrationFormData } from '../../utils/formValidation';
import { useEntity } from '../../hooks/useEntity';
import { EntityFormTemplate } from '../../components/forms/EntityFormTemplate';
import { FormFieldWrapper } from '../../components/forms/FormFieldWrapper';
import { DynamicFieldArray } from '../../components/forms/DynamicFieldArray';
import { DiscardChangesModal } from '../../components/forms/DiscardChangesModal';
import { hasUnsavedChanges, deepClone } from '../../utils/formHelpers';
import './EntityFormPage.css';

interface IntegrationFormPageProps {
  isEdit?: boolean;
}

export function IntegrationFormPage({ isEdit = false }: IntegrationFormPageProps) {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [showDiscardModal, setShowDiscardModal] = useState(false);

  const { data: existingIntegration, isLoading: isLoadingIntegration } = useEntity(
    isEdit ? 'integrations' : '',
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
  } = useForm<IntegrationFormData>({
    resolver: zodResolver(IntegrationFormSchema),
    defaultValues: isEdit
      ? existingIntegration || {}
      : {
          name: '',
          description: '',
          sourceSystem: '',
          targetSystem: '',
          protocol: 'REST',
          frequency: 'Daily',
          sla: 99.9,
          direction: 'Unidirectional',
          owner: '',
          dataClassification: 'Internal',
          errorThreshold: 5,
          retryPolicy: '',
          tags: [],
        },
  });

  const { field: tagsField } = useController({
    control,
    name: 'tags',
  });

  const watchedValues = watch();
  const [originalValues] = useState(() => deepClone(watchedValues));
  const hasChanges = hasUnsavedChanges(originalValues, watchedValues);

  useEffect(() => {
    if (existingIntegration) {
      reset(existingIntegration);
    }
  }, [existingIntegration, reset]);

  const onSubmit = async (data: IntegrationFormData) => {
    try {
      let response;

      if (isEdit && id) {
        response = await apiClient.patch(`/integrations/${id}`, data);
      } else {
        response = await apiClient.post('/integrations', data);
      }

      if (response.data?.id) {
        navigate(`/entities/integrations/${response.data.id}`);
      }
    } catch (error: any) {
      const errorMessage =
        error?.response?.data?.detail || error?.message || 'Failed to save integration';

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
      navigate('/entities/integrations');
    }
  };

  const handleConfirmDiscard = () => {
    setShowDiscardModal(false);
    navigate('/entities/integrations');
  };

  if (isEdit && isLoadingIntegration) {
    return <div className="entity-form-loading">Loading...</div>;
  }

  return (
    <>
      <EntityFormTemplate
        title={isEdit ? 'Edit Integration' : 'Create Integration'}
        subtitle={
          isEdit ? `Editing: ${existingIntegration?.name || ''}` : 'Add a new integration'
        }
        isEdit={isEdit}
        isLoading={isSubmitting}
        error={null}
        onSubmit={handleReactFormSubmit(onSubmit)}
        onCancel={handleCancel}
        submitButtonLabel={isEdit ? 'Save Changes' : 'Create Integration'}
      >
        <div className="entity-form-section">
          <h3 className="entity-form-section-title">Basic Information</h3>

          <div className="entity-form-section-grid">
            <FormFieldWrapper
              label="Integration Name"
              required
              error={errors.name?.message}
              htmlFor="name"
            >
              <input
                id="name"
                type="text"
                placeholder="e.g., CRM to ERP Sync"
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
                placeholder="Enter owner user ID"
                {...register('owner')}
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
              placeholder="Describe the integration's purpose..."
              {...register('description')}
            />
          </FormFieldWrapper>
        </div>

        <div className="entity-form-section">
          <h3 className="entity-form-section-title">Integration Configuration</h3>

          <div className="entity-form-section-grid">
            <FormFieldWrapper
              label="Source System"
              required
              error={errors.sourceSystem?.message}
              htmlFor="sourceSystem"
            >
              <input
                id="sourceSystem"
                type="text"
                placeholder="e.g., Salesforce"
                {...register('sourceSystem')}
              />
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Target System"
              required
              error={errors.targetSystem?.message}
              htmlFor="targetSystem"
            >
              <input
                id="targetSystem"
                type="text"
                placeholder="e.g., SAP"
                {...register('targetSystem')}
              />
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Protocol"
              required
              error={errors.protocol?.message}
              htmlFor="protocol"
            >
              <select id="protocol" {...register('protocol')}>
                <option value="REST">REST</option>
                <option value="SOAP">SOAP</option>
                <option value="GraphQL">GraphQL</option>
                <option value="Database">Database</option>
                <option value="File">File</option>
                <option value="Message">Message</option>
              </select>
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Frequency"
              required
              error={errors.frequency?.message}
              htmlFor="frequency"
            >
              <select id="frequency" {...register('frequency')}>
                <option value="RealTime">Real-time</option>
                <option value="Hourly">Hourly</option>
                <option value="Daily">Daily</option>
                <option value="Weekly">Weekly</option>
                <option value="Monthly">Monthly</option>
              </select>
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Direction"
              required
              error={errors.direction?.message}
              htmlFor="direction"
            >
              <select id="direction" {...register('direction')}>
                <option value="Unidirectional">Unidirectional</option>
                <option value="Bidirectional">Bidirectional</option>
              </select>
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Data Classification"
              required
              error={errors.dataClassification?.message}
              htmlFor="dataClassification"
            >
              <select id="dataClassification" {...register('dataClassification')}>
                <option value="Public">Public</option>
                <option value="Internal">Internal</option>
                <option value="Confidential">Confidential</option>
              </select>
            </FormFieldWrapper>
          </div>

          <div className="entity-form-section-grid">
            <FormFieldWrapper
              label="SLA (%)"
              required
              error={errors.sla?.message}
              htmlFor="sla"
              helpText="99.0 - 99.99"
            >
              <input
                id="sla"
                type="number"
                step="0.01"
                placeholder="99.9"
                {...register('sla', { valueAsNumber: true })}
              />
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Error Threshold (%)"
              error={errors.errorThreshold?.message}
              htmlFor="errorThreshold"
            >
              <input
                id="errorThreshold"
                type="number"
                min="0"
                max="100"
                placeholder="5"
                {...register('errorThreshold', { valueAsNumber: true })}
              />
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Retry Policy"
              error={errors.retryPolicy?.message}
              htmlFor="retryPolicy"
            >
              <input
                id="retryPolicy"
                type="text"
                placeholder="e.g., exponential-backoff"
                {...register('retryPolicy')}
              />
            </FormFieldWrapper>
          </div>

          <DynamicFieldArray
            label="Tags"
            value={tagsField.value || []}
            onChange={tagsField.onChange}
            placeholder="e.g., critical, crm"
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
