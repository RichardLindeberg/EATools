import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useController, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { apiClient } from '../../api/client';
import { BusinessCapabilityFormSchema, BusinessCapabilityFormData } from '../../utils/formValidation';
import { useEntity } from '../../hooks/useEntity';
import { EntityFormTemplate } from '../../components/forms/EntityFormTemplate';
import { FormFieldWrapper } from '../../components/forms/FormFieldWrapper';
import { DiscardChangesModal } from '../../components/forms/DiscardChangesModal';
import { hasUnsavedChanges, deepClone } from '../../utils/formHelpers';
import { updateBusinessCapabilityWithCommands } from '../../utils/commandDispatcher';
import './EntityFormPage.css';

interface BusinessCapabilityFormPageProps {
  isEdit?: boolean;
}

export function BusinessCapabilityFormPage({ isEdit = false }: BusinessCapabilityFormPageProps) {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [showDiscardModal, setShowDiscardModal] = useState(false);
  const [originalValues, setOriginalValues] = useState<BusinessCapabilityFormData | null>(null);

  const { data: existingCapability, isLoading: isLoadingCapability } = useEntity(
    isEdit ? 'business-capabilities' : '',
    isEdit ? id : ''
  );

  const {
    register,
    watch,
    handleSubmit: handleReactFormSubmit,
    formState: { errors, isSubmitting },
    setError,
    reset,
  } = useForm<BusinessCapabilityFormData>({
    resolver: zodResolver(BusinessCapabilityFormSchema),
    defaultValues: isEdit
      ? existingCapability || {}
      : {
          name: '',
          description: '',
          owner: '',
          status: 'Active',
          strategicValue: 'High',
          architectureStyle: 'Microservice',
          currentState: '',
          targetState: '',
          performanceKpi: '',
          timeline: '',
          supportingApplications: [],
          parent: null,
        },
  });

  const watchedValues = watch();
  const hasChanges = originalValues ? hasUnsavedChanges(originalValues, watchedValues) : false;

  useEffect(() => {
    if (existingCapability) {
      reset(existingCapability);
      setOriginalValues(deepClone(existingCapability as BusinessCapabilityFormData));
    }
  }, [existingCapability, reset]);

  useEffect(() => {
    if (!isEdit) {
      const defaults: BusinessCapabilityFormData = {
        name: '',
        description: '',
        owner: '',
        status: 'Active',
        strategicValue: 'High',
        architectureStyle: 'Microservice',
        currentState: '',
        targetState: '',
        performanceKpi: '',
        timeline: '',
        supportingApplications: [],
        parent: null,
      };
      setOriginalValues(defaults);
    }
  }, [isEdit]);

  const onSubmit = async (data: BusinessCapabilityFormData) => {
    try {
      let response;

      if (isEdit && id) {
        const result = await updateBusinessCapabilityWithCommands(
          id,
          originalValues || {},
          data
        );
        if (result?.id) {
          navigate(`/entities/business-capabilities/${result.id}`);
        }
      } else {
        response = await apiClient.post('/business-capabilities', data);
        if (response.data?.id) {
          navigate(`/entities/business-capabilities/${response.data.id}`);
        }
      }
    } catch (error: any) {
      const errorMessage =
        error?.response?.data?.detail || error?.message || 'Failed to save business capability';

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
      navigate('/entities/business-capabilities');
    }
  };

  const handleConfirmDiscard = () => {
    setShowDiscardModal(false);
    navigate('/entities/business-capabilities');
  };

  if (isEdit && isLoadingCapability) {
    return <div className="entity-form-loading">Loading...</div>;
  }

  return (
    <>
      <EntityFormTemplate
        title={isEdit ? 'Edit Business Capability' : 'Create Business Capability'}
        subtitle={
          isEdit
            ? `Editing: ${existingCapability?.name || ''}`
            : 'Add a new business capability'
        }
        isEdit={isEdit}
        isLoading={isSubmitting}
        error={null}
        onSubmit={handleReactFormSubmit(onSubmit)}
        onCancel={handleCancel}
        submitButtonLabel={isEdit ? 'Save Changes' : 'Create Capability'}
      >
        <div className="entity-form-section">
          <h3 className="entity-form-section-title">Basic Information</h3>

          <div className="entity-form-section-grid">
            <FormFieldWrapper
              label="Capability Name"
              required
              error={errors.name?.message}
              htmlFor="name"
            >
              <input
                id="name"
                type="text"
                placeholder="e.g., Order Management"
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
              label="Status"
              required
              error={errors.status?.message}
              htmlFor="status"
            >
              <select id="status" {...register('status')}>
                <option value="Planned">Planned</option>
                <option value="Building">Building</option>
                <option value="Active">Active</option>
                <option value="Retiring">Retiring</option>
              </select>
            </FormFieldWrapper>
          </div>

          <FormFieldWrapper
            label="Description"
            error={errors.description?.message}
            htmlFor="description"
          >
            <textarea
              id="description"
              placeholder="Describe the business capability..."
              {...register('description')}
            />
          </FormFieldWrapper>
        </div>

        <div className="entity-form-section">
          <h3 className="entity-form-section-title">Strategic Information</h3>

          <div className="entity-form-section-grid">
            <FormFieldWrapper
              label="Strategic Value"
              error={errors.strategicValue?.message}
              htmlFor="strategicValue"
            >
              <select id="strategicValue" {...register('strategicValue')}>
                <option value="Critical">Critical</option>
                <option value="High">High</option>
                <option value="Medium">Medium</option>
                <option value="Low">Low</option>
              </select>
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Architecture Style"
              error={errors.architectureStyle?.message}
              htmlFor="architectureStyle"
            >
              <select id="architectureStyle" {...register('architectureStyle')}>
                <option value="Microservice">Microservice</option>
                <option value="Monolith">Monolith</option>
                <option value="Hybrid">Hybrid</option>
              </select>
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Parent Capability"
              error={errors.parent?.message}
              htmlFor="parent"
            >
              <input
                id="parent"
                type="text"
                placeholder="Parent capability ID (optional)"
                {...register('parent')}
              />
            </FormFieldWrapper>
          </div>

          <div className="entity-form-section-grid">
            <FormFieldWrapper
              label="Current State"
              error={errors.currentState?.message}
              htmlFor="currentState"
            >
              <input
                id="currentState"
                type="text"
                placeholder="e.g., Partial"
                {...register('currentState')}
              />
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Target State"
              error={errors.targetState?.message}
              htmlFor="targetState"
            >
              <input
                id="targetState"
                type="text"
                placeholder="e.g., Fully Automated"
                {...register('targetState')}
              />
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Timeline"
              error={errors.timeline?.message}
              htmlFor="timeline"
            >
              <input
                id="timeline"
                type="text"
                placeholder="e.g., Q2 2026"
                {...register('timeline')}
              />
            </FormFieldWrapper>
          </div>

          <FormFieldWrapper
            label="Performance KPI"
            error={errors.performanceKpi?.message}
            htmlFor="performanceKpi"
          >
            <input
              id="performanceKpi"
              type="text"
              placeholder="e.g., 99.9% availability"
              {...register('performanceKpi')}
            />
          </FormFieldWrapper>
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
